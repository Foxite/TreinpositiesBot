import {Component, Input, Output} from '@angular/core';
import {LevelInfo} from "../../models/models";

@Component({
  selector: 'app-config-form',
  templateUrl: './config-form.component.html',
  styleUrls: ['./config-form.component.scss']
})
export class ConfigFormComponent {
  @Input() level: LevelInfo | null = null;

  protected readonly JSON = JSON;
}
