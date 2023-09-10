import {Component, Input} from '@angular/core';
import {ConfigFormElement} from "../config-form.component";
import {ConfigKeyService} from "../../../services/config-key.service";
import {LevelInfo} from "../../../models/models";

@Component({
  selector: 'app-config-form-element',
  templateUrl: './config-form-element.component.html',
  styleUrls: ['./config-form-element.component.scss']
})
export class ConfigFormElementComponent {
  @Input() element!: ConfigFormElement;
  @Input() level!: LevelInfo;
  //@Output() change = new EventEmitter(Event)

  constructor(private configService: ConfigKeyService) {
  }

  onChange(event: Event) {
    console.log((event.target as HTMLInputElement).value);
    this.configService.setConfigKey(this.level, this.element.key, (event.target as HTMLInputElement).value)
      .catch(error => console.error(error));
  }
}
