import { Component } from '@angular/core';
import {Observable} from "rxjs";
import {GuildInfo} from "../../models/models";
import {ChannelConfigService} from "../../services/channel-config.service";

@Component({
  selector: 'app-guilds',
  templateUrl: './guilds.component.html',
  styleUrls: ['./guilds.component.scss']
})
export class GuildsComponent {
  guilds!: GuildInfo[];
  currentGuild!: GuildInfo | null;

  constructor(private ccs: ChannelConfigService) {
  }

  ngOnInit(): void {
    this.guilds = Object.values(this.ccs.getGuilds());
  }

  setCurrentGuild(guild: GuildInfo) {
    this.currentGuild = guild;
  }
}
