import {Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
import {LevelInfo} from "../../models/models";
import {ConfigFormElementComponent} from "./config-form-element/config-form-element.component";

@Component({
  selector: 'app-config-form',
  templateUrl: './config-form.component.html',
  styleUrls: ['./config-form.component.scss']
})
export class ConfigFormComponent implements OnInit, OnChanges {
  @Input() level: LevelInfo | null = null;
  @Output() hoveredElementSpecifierPath = new EventEmitter<string>();

  elements: ConfigFormElement[] | null = null;

  private setFormElements() {
    this.elements = [
      {
        type: ConfigFormElementType.Number,
        key: "Cooldown",
        label: "Cooldown (seconds)",
      },
      {
        type: ConfigFormElementType.SelectMultiple,
        key: "SourceNames",
        label: "Sources",
        validation: ["Treinposities", "Busposities", "Trein/Busposities", "Planespotters"],
      },
    ]
  }

  ngOnInit() {
    this.setFormElements();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes["level"]) {
      this.setFormElements();
    }
  }

  hoverElement(hElement: ConfigFormElementComponent) {
    this.hoveredElementSpecifierPath.emit(hElement.valueSpecifiedAt);
  }
}

export interface ConfigFormElement {
  type: ConfigFormElementType,
  key: string,
  label: string,
  validation?: any,
}

export enum ConfigFormElementType {
  Text = 'text',
  Number = 'number',
  SelectSingle = 'select-single',
  SelectMultiple = 'select-multiple',
}
