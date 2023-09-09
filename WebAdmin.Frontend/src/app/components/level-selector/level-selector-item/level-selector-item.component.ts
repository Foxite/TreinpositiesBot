import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {LevelInfo} from "../../../models/models";

@Component({
  selector: 'app-level-selector-item',
  templateUrl: './level-selector-item.component.html',
  styleUrls: ['./level-selector-item.component.scss']
})
export class LevelSelectorItemComponent implements OnInit {
  @Output() selected = new EventEmitter<LevelInfo>();
  @Input() myLevel!: LevelInfo;
  @Input() currentLevel!: LevelInfo | null;

  children!: LevelInfo[];

  ngOnInit() {
    console.log(this.myLevel);
    if (this.myLevel.children) {
      this.children = Object.values(this.myLevel.children);
    } else {
      this.children = [];
    }
  }

  onClick(event: MouseEvent) {
    this.selected.emit(this.myLevel);
    event.stopPropagation();
  }

  onChildSelected(level: LevelInfo) {
    this.selected.emit(level);
  }
}
