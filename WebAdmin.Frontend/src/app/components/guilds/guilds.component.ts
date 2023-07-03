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
  guilds!: Promise<GuildInfo[]>;
  currentGuildId!: number | null;

  constructor(private ccs: ChannelConfigService) {
  }

  ngOnInit(): void {
    this.guilds = this.ccs.getGuilds();
  }

  setCurrentGuildId(guildId: number) {
    this.currentGuildId = guildId;
  }
}
