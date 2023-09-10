import {Component, EventEmitter, Input, OnInit, Optional, Output} from '@angular/core';
import {ConfigFormElement, ConfigFormElementType} from "../config-form.component";
import {ConfigKeyService} from "../../../services/config-key.service";
import {LevelInfo} from "../../../models/models";

@Component({
  selector: 'app-config-form-element',
  templateUrl: './config-form-element.component.html',
  styleUrls: ['./config-form-element.component.scss']
})
export class ConfigFormElementComponent implements OnInit {
  @Input() element!: ConfigFormElement;
  @Input() level!: LevelInfo;
  //@Output() hover = new EventEmitter<ConfigFormElementComponent>();

  //@Output() change = new EventEmitter(Event)
  elementValue: any | undefined = undefined;
  valueSpecifiedAt: string | undefined = undefined;
  displayState = DisplayState.Loading;

  constructor(private configService: ConfigKeyService) {
  }

  async ngOnInit() {
    try {
      const value = await this.configService.getConfigKey(this.level, this.element.key);
      if (value) {
        this.elementValue = value.value
        this.valueSpecifiedAt = value.overrideLevel;
      }

      this.displayState = DisplayState.Ready;
    } catch (error) {
      console.error(error);
      this.displayState = DisplayState.Error;
    }
  }

  async deleteItem() {
    try {
      this.displayState = DisplayState.Saving;
      await this.configService.deleteConfigKey(this.level, this.element.key);

      const value = await this.configService.getConfigKey(this.level, this.element.key);
      if (value) {
        this.elementValue = value.value
        this.valueSpecifiedAt = value.overrideLevel;
      }

      this.displayState = DisplayState.Saved;

      setTimeout(() => this.displayState = DisplayState.Ready, 2500);
    } catch (error) {
      console.error(error);
      this.displayState = DisplayState.Error;
    }
  }

  onChange(event: Event) {
    this.displayState = DisplayState.Saving;

    switch (this.element.type) {
      case ConfigFormElementType.Number:
      case ConfigFormElementType.Text:
      case ConfigFormElementType.SelectSingle:
        this.elementValue = (event.target as HTMLInputElement).value;
        break;
      case ConfigFormElementType.SelectMultiple:
        this.elementValue = Array.from((event.target as HTMLSelectElement).selectedOptions).map(option => option.value);
        break;
    }

    console.log(event);
    this.configService.setConfigKey(this.level, this.element.key, this.elementValue)
      .then(() => {
        this.displayState = DisplayState.Saved;
        this.valueSpecifiedAt = this.level.path;

        setTimeout(() => this.displayState = DisplayState.Ready, 2500);
      })
      .catch(error => {
        console.error(error);
        this.displayState = DisplayState.Error;
      });
  }

  protected readonly ConfigFormElementType = ConfigFormElementType;
  protected readonly undefined = undefined;
}

export enum DisplayState {
  Loading = 'loading',
  Saving = 'saving',
  Saved = 'saved',
  Ready = 'ready',
  Error = 'error',
}
