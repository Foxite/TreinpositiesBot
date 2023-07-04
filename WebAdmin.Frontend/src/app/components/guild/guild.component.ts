import {Component, Input, OnChanges, OnInit, SimpleChanges} from '@angular/core';
import {GuildConfig, GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";
import {GuildsComponent} from "../guilds/guilds.component";
import {ActivatedRoute} from "@angular/router";
import {SecurityService} from "../../services/security/security.service";

@Component({
  selector: 'app-guild',
  templateUrl: './guild.component.html',
  styleUrls: ['./guild.component.scss']
})
export class GuildComponent implements OnInit {
  guildId!: number | null;
  guild!: GuildInfo | null;

  guildConfig!: GuildConfig | null;

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService,
              private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.security.userObservable().subscribe(user => {
      this.updateGuild();
    });

    this.route.paramMap.subscribe( paramMap => {
      this.updateGuild();
    })
  }

  updateGuild() {
    const guildId = this.route.snapshot.params["guildId"];

    if (!guildId) {
      this.guildId = null;
      this.guild = null;
      return;
    }

    this.guildId = parseInt(guildId);
    // this triggers the ngOnChanges below

    if (!this.security.currentUser()) {
      return;
    }

    this.guild = this.security.currentUser()!.guilds[this.guildId];

    // TODO show spinner
    this.ccs.getGuild(this.guildId)
      .then(gc => {
        // TODO remove spinner
        this.guildConfig = gc;
      })
      .catch(error => {
        console.error(error);
        // TODO show error
      });
  }

  protected readonly JSON = JSON;
}
