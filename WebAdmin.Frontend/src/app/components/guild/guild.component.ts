import {Component, Input, OnChanges, OnInit, SimpleChanges} from '@angular/core';
import {GuildConfig, GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {GuildsComponent} from "../guilds/guilds.component";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-guild',
  templateUrl: './guild.component.html',
  styleUrls: ['./guild.component.scss']
})
export class GuildComponent implements OnInit {
  guildId!: number;
  guild!: GuildInfo | null;

  guildConfig!: GuildConfig | null;

  constructor(private ccs: ChannelConfigService,
              private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.updateGuildId(this.route.snapshot.params["guildId"]);

    this.route.paramMap.subscribe( paramMap => {
      this.updateGuildId(paramMap.get("guildId")!);
    })
  }

  updateGuildId(guildId: string) {
    console.log(guildId);

    this.guildId = parseInt(guildId);
    // this triggers the ngOnChanges below
    this.guild = this.ccs.getGuilds()[this.guildId];

    // TODO show spinner
    this.ccs.getGuild(this.guildId)
      .then(gc => {
        // TODO remove spinner
        console.log(gc);
        this.guildConfig = gc;
      })
      .catch(error => {
        console.error(error);
        // TODO show error
      });
  }

  protected readonly JSON = JSON;
}
