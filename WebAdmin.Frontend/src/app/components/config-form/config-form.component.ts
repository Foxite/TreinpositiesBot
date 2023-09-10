import {Component, Input, OnInit, Output} from '@angular/core';
import {LevelInfo} from "../../models/models";
import {ConfigKeyService} from "../../services/config-key.service";

@Component({
  selector: 'app-config-form',
  templateUrl: './config-form.component.html',
  styleUrls: ['./config-form.component.scss']
})
export class ConfigFormComponent implements OnInit {
  @Input() level: LevelInfo | null = null;

  elements: ConfigFormElement[] | null = null;

  ngOnInit() {
    this.elements = [
      {
        type: ConfigFormElementType.Number,
        key: "Cooldown",
        placeholder: "placeholder",
        label: "Cooldown (seconds)"
      },
      {
        type: ConfigFormElementType.Text,
        key: "SourceNames",
        placeholder: "[]",
        label: "Source names"
      },
    ]
  }
}

export interface ConfigFormElement {
  type: ConfigFormElementType,
  key: string,
  label: string,
  placeholder: string,
}

export enum ConfigFormElementType {
  Text = 'text',
  Number = 'number',
}
