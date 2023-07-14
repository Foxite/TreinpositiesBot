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

  itemConfig!: (GuildConfig | GuildChannelConfig) | undefined;

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

    if (!this.security.currentUser()) {
      return;
    }

    this.guild = this.security.currentUser()!.guilds[this.guildId!];

    // TODO show spinner
    this.ccs.getGuild(this.guildId)
      .then(gc => {
        // TODO remove spinner
        this.guildConfig = gc;
        if (this.guildConfig === null) {
          // TODO show "bot is not in channel" error
          console.log("gc is null");
        }
      })
      .catch(error => {
        console.error(error);
        // TODO show error
      });
  }

  updateSelectedItem() {
    const newItemId = this.route.snapshot.params["channelId"];

    // If the new id and the current item's id are the same, accounting for the current item being null and the new id being unspecified
    if (((!newItemId || newItemId == "0") && this.itemConfig == null) || newItemId == this.itemConfig?.id) {
      return;
    }

    if (!this.guildConfig) {
      return;
    }

    if (newItemId === null) {
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
