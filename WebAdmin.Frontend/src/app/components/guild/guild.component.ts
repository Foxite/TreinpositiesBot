import {Component, OnInit} from '@angular/core';
import {GuildChannelConfig, GuildConfig, GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {ActivatedRoute, EventType, Router} from "@angular/router";
import {SecurityService} from "../../services/security/security.service";
import {DiscordService} from "../../services/discord/discord.service";

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

  itemConfig!: (GuildConfig | GuildChannelConfig) | undefined;

  protected readonly DisplayState = DisplayState;

  // For debugging only
  protected readonly JSON = JSON;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService,
              private discord: DiscordService,
              private router: Router,
              private route: ActivatedRoute) {
  }

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

  updateGuild() {
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
    } else {
      for (const category of this.guildConfig?.categories) {
        this.itemConfig = category.channels.find(channel => channel.id == newItemId);
        if (this.itemConfig != null) {
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
