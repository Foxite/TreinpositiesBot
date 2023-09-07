import { Component } from '@angular/core';

@Component({
  selector: 'app-config-panel',
  templateUrl: './config-panel.component.html',
  styleUrls: ['./config-panel.component.scss']
})
export class ConfigPanelComponent {
  currentRootLevelId: string | null = null;

  onSelectRootLevel(rootLevelId: string) {
    console.log(rootLevelId);
    this.currentRootLevelId = rootLevelId;
  }
}
