import {Component, Input} from '@angular/core';
import {GuildConfig} from "../../models/models";

@Component({
  selector: 'app-guild',
  templateUrl: './guild.component.html',
  styleUrls: ['./guild.component.scss']
})
export class GuildComponent {
  @Input() guildConfig!: GuildConfig;
}
