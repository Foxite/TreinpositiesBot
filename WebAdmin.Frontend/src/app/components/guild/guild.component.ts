import {Component, OnInit} from '@angular/core';
import {GuildConfig, GuildInfo, ItemConfig} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {ActivatedRoute, Router} from "@angular/router";
import {SecurityService} from "../../services/security/security.service";
import {DiscordService} from "../../services/discord/discord.service";
import {FormControl, FormGroup} from "@angular/forms";

@Component({
  selector: 'app-guild',
  templateUrl: './guild.component.html',
  styleUrls: ['./guild.component.scss']
})
export class GuildComponent implements OnInit {
  guildId!: string | null;
  guild!: GuildInfo | null;
  guildConfig!: GuildConfig | null;
  displayState: DisplayState = DisplayState.NoGuildSelected;

  itemConfig!: ItemConfig | undefined;
  itemDisplayName!: string;
  itemConfigForm = new FormGroup({
    specifyCooldown: new FormControl(false),
    cooldown: new FormControl(0),
    specifySources: new FormControl(false),
    sources: new FormControl([]),
  });

  protected readonly DisplayState = DisplayState;

  // For debugging only
  protected readonly JSON = JSON;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService,
              private discord: DiscordService,
              private router: Router,
              private route: ActivatedRoute,
  ) {}

  ngOnInit() {
    this.security.userObservable().subscribe(user => {
      this.updateGuild();
    });

    this.route.paramMap.subscribe(paramMap => {
      this.updateGuild();
      this.updateSelectedItem();
    });

    this.updateGuild();
    this.updateSelectedItem();
  }

  private buildItemConfigFromForm(): { cooldownSeconds: number | null, sourceNames: string[] | null } {
    return {
      cooldownSeconds: this.itemConfigForm.get("specifyCooldown")!.value === false ? null : this.itemConfigForm.get("cooldown")!.value,
      sourceNames: this.itemConfigForm.get("specifySources")!.value === false ? null : this.itemConfigForm.get("sources")!.value,
    };
  }

  static arrayEquals<T>(a: T[] | null, b: T[] | null): boolean {
    return (a == null && b == null) || (a != null && b != null && a.length === b.length && a.every((val, index) => val === b[index]));
  }

  async onFormInput(): Promise<void> {
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
      if (this.route.snapshot.params["channelId"] === "0") {
        await this.ccs.setGuildCooldown(this.guildId, newConfig.cooldownSeconds);
      } else {
        await this.ccs.setChannelCooldown(this.guildId, this.route.snapshot.params["channelId"], newConfig.cooldownSeconds);
      }
    }

    if (!GuildComponent.arrayEquals(newConfig.sourceNames, this.itemConfig.sourceNames)) {
      if (this.route.snapshot.params["channelId"] === "0") {
        await this.ccs.setGuildCooldown(this.guildId, newConfig.cooldownSeconds);
      } else {
        await this.ccs.setChannelSources(this.guildId, this.route.snapshot.params["channelId"], newConfig.sourceNames);
      }
    }
  }

  updateGuild() {
    // TODO use component input parameter
    const newGuildId = this.route.snapshot.params["guildId"];

    if (newGuildId == this.guildId) {
      return;
    }

    this.guildId = newGuildId;
    if (!this.guildId || this.guildId == "0") {
      this.guild = null;
      return;
    }

    const currentUser = this.security.currentUser();
    if (!currentUser) {
      return;
    }

    this.guild = currentUser.guilds[this.guildId!];
    this.displayState = DisplayState.LoadingGuild;

    this.ccs.getGuild(this.guildId)
      .then(gc => {
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
    } else {
      for (const category of this.guildConfig?.categories) {
        const channel = category.channels.find(channel => channel.id == newItemId);
        if (channel != null) {
          this.itemConfig = channel;
          this.itemDisplayName = this.guild!.name + " / #" + channel.name + " configuration";
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
