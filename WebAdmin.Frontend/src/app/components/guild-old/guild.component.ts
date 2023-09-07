import {Component, OnInit} from '@angular/core';
import {GuildConfig, GuildInfo, ItemConfig} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {ActivatedRoute, Router} from "@angular/router";
import {SecurityService} from "../../services/security/security.service";
import {DiscordService} from "../../services/discord/discord.service";
import {debounceTime, Subject} from "rxjs";

class FormModel {
  specifyCooldown: boolean = false;
  cooldown: number = 0;
  specifySources: boolean = false;
  sources: string[] = [];
}

@Component({
  selector: 'app-guild',
  templateUrl: './guild.component.html',
  styleUrls: ['./guild.component.scss']
})
export class GuildComponent implements OnInit {
  readonly availableSourceNames = [
    "Treinposities",
    "Busposities",
    "Planespotters"
  ]

  guildId!: string | null;
  guild!: GuildInfo | null;
  guildConfig!: GuildConfig | null;
  displayState: DisplayState = DisplayState.NoGuildSelected;

  itemConfig!: ItemConfig | undefined;
  itemDisplayName!: string;

  // TODO populate with item config
  itemConfigForm: FormModel = new FormModel();

  protected readonly DisplayState = DisplayState;

  // For debugging only
  protected readonly JSON = JSON;

  //formInputEvent!: Observable<void>;
  formInputEventSource = new Subject<void>;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService,
              private discord: DiscordService,
              private router: Router,
              private route: ActivatedRoute,
  ) {
  }

  ngOnInit() {
    this.formInputEventSource
      .pipe(debounceTime(500))
      .subscribe(() => this.processFormInput());

    this.security.userObservable.subscribe(user => {
      this.updateGuild();
    });

    this.route.paramMap.subscribe(paramMap => {
      this.updateGuild();
      this.updateSelectedItem();
    });

    //this.updateGuild();
    //this.updateSelectedItem();
  }

  private buildItemConfigFromForm(): { cooldownSeconds: number | null, sourceNames: string[] | null } {
    return {
      cooldownSeconds: !this.itemConfigForm.specifyCooldown ? null : this.itemConfigForm.cooldown,
      sourceNames: !this.itemConfigForm.specifySources ? null : this.itemConfigForm.sources,
    };
  }

  private buildFormFromItemConfig(): FormModel {
    return {
      specifyCooldown: this.itemConfig?.cooldownSeconds !== null,
      cooldown: this.itemConfig?.cooldownSeconds || 0,
      specifySources: this.itemConfig?.sourceNames !== null,
      sources: this.itemConfig?.sourceNames || [],
    };
  }

  static arrayEquals<T>(a: T[] | null, b: T[] | null): boolean {
    return (a == null && b == null) || (a != null && b != null && a.length === b.length && a.every((val, index) => val === b[index]));
  }

  onFormInput(): void {
    this.formInputEventSource.next();
  }

  async processFormInput(): Promise<void> {
    if (!this.itemConfig) {
      console.error("Called onFormInput while itemConfig is undefined");
      return;
    }

    if (!this.guildId) {
      console.error("Called onFormInput while guildId is undefined");
      return;
    }

    // TODO debounce
    const newConfig = this.buildItemConfigFromForm();
    if (newConfig.cooldownSeconds !== this.itemConfig.cooldownSeconds) {
      console.log("update cooldown");
      if (this.route.snapshot.params["channelId"] === "0") {
        await this.ccs.setGuildCooldown(this.guildId, newConfig.cooldownSeconds);
      } else {
        await this.ccs.setChannelCooldown(this.guildId, this.route.snapshot.params["channelId"], newConfig.cooldownSeconds);
      }
      this.itemConfig.cooldownSeconds = newConfig.cooldownSeconds;
    }

    const doesEqual = !GuildComponent.arrayEquals(newConfig.sourceNames, this.itemConfig.sourceNames);
    if (doesEqual) {
      console.log("update sources");
      if (this.route.snapshot.params["channelId"] === "0") {
        await this.ccs.setGuildSources(this.guildId, newConfig.sourceNames);
      } else {
        await this.ccs.setChannelSources(this.guildId, this.route.snapshot.params["channelId"], newConfig.sourceNames);
      }
      this.itemConfig.sourceNames = newConfig.sourceNames;
    }
  }

  updateGuild() {
    // TODO use component input parameter
    const newGuildId = this.route.snapshot.params["guildId"];

    const currentUser = this.security.currentUser;
    if (!currentUser) {
      return;
    }

    if (newGuildId == this.guildId) {
      return;
    }

    this.guildId = newGuildId;
    if (!this.guildId || this.guildId == "0") {
      this.guild = null;
      return;
    }

    this.guild = currentUser.guilds[this.guildId!];
    this.displayState = DisplayState.LoadingGuild;

    this.ccs.getGuild(this.guildId)
      .then(gc => {
        if (this.guildId != newGuildId) {
          // this.guildId was changed before this promise resolved.
          return;
        }
        this.guildConfig = gc;
        if (this.guildConfig === null) {
          this.displayState = DisplayState.BotNotInGuild;
        } else {
          this.displayState = DisplayState.GuildSelected;
          this.updateSelectedItem();
        }
      })
      .catch(error => {
        console.error(error);
        this.displayState = DisplayState.BotNotInGuild;
      });
  }

  updateSelectedItem() {
    const newItemId = this.route.snapshot.params["channelId"];

    // If the new id and the current item's id are the same, accounting for the current item being null and the new id being unspecified
    if ((!newItemId && this.itemConfig == null) || newItemId == this.itemConfig?.id) {
      return;
    }

    if (!this.guildConfig) {
      return;
    }

    if (newItemId === "0") {
      this.itemConfig = this.guildConfig;
      this.itemDisplayName = this.guild!.name + " server-wide configuration";
      this.itemConfigForm = this.buildFormFromItemConfig();
    } else {
      for (const category of this.guildConfig?.categories) {
        const channel = category.channels.find(channel => channel.id == newItemId);
        if (channel != null) {
          this.itemConfig = channel;
          this.itemDisplayName = this.guild!.name + " / #" + channel.name + " configuration";
          this.itemConfigForm = this.buildFormFromItemConfig();
          return;
        }
      }
    }
  }
}

export enum DisplayState {
  NoGuildSelected,
  LoadingGuild,
  ErrorLoadingGuild,
  BotNotInGuild,
  GuildSelected
}
