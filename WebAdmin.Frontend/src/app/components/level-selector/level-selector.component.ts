import {Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
import {LevelInfo, RootLevelInfo} from "../../models/models";
import {SecurityService} from "../../services/security/security.service";
import {LevelsService} from "../../services/levels/levels.service";
import {getLevelFromPath} from "../../../util";

@Component({
  selector: 'app-level-selector',
  templateUrl: './level-selector.component.html',
  styleUrls: ['./level-selector.component.scss']
})
export class LevelSelectorComponent implements OnInit, OnChanges {
  @Input() levelPath: string | null = null;
  @Input() highlightedPath: string | null = null;
  @Output() selected = new EventEmitter<LevelInfo>();

  currentLevel: LevelInfo | null = null;
  displayState: DisplayState = DisplayState.NoRootLevelSelected;

  rootLevel: RootLevelInfo | null = null;
  levelRoot: LevelInfo | null = null; // these represent the same thing, but rootLevel resolves immediately, while levelRoot contains more info and is slow to retrieve

  protected readonly DisplayState = DisplayState;

  constructor(private security: SecurityService,
              private levelsService: LevelsService,
  ) { }

  ngOnInit() {
    this.security.userObservable.subscribe(user => {
      if (this.levelPath !== null) {
        this.updateLevels(null, this.levelPath);
      }
    });

    /*
    this.route.paramMap.subscribe(paramMap => {
      this.updateGuild();
    });
     */
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes["levelPath"]) {
      this.updateLevels(changes["levelPath"].previousValue, changes["levelPath"].currentValue);
    }
  }

  private updateLevels(oldLevelPath: string | null, newLevelPath: string) {
    const currentUser = this.security.currentUser;
    if (!currentUser) {
      return;
    }

    if (oldLevelPath == newLevelPath) {
      return;
    }

    const oldRootId = oldLevelPath?.split(':')[0];
    const newRootId = newLevelPath.split(':')[0];

    if (oldRootId == newRootId) {
      this.setSelectedLevel(newLevelPath);
      return;
    }

    this.rootLevel = currentUser.rootLevels[newRootId!];
    this.displayState = DisplayState.LoadingLevelRoot;

    this.levelsService.getLevelTree(newRootId)
      .then(levelRoot => {
        if (this.levelPath !== newLevelPath) {
          // this.guildId was changed before this promise resolved.
          return;
        }

        this.levelRoot = levelRoot;
        if (this.levelRoot === null) {
          this.displayState = DisplayState.LevelRootNotAvailable;
        } else {
          this.displayState = DisplayState.LevelRootLoaded;
          this.setSelectedLevel(newLevelPath);
        }
      })
      .catch(error => {
        console.error(error);
        this.displayState = DisplayState.LevelRootNotAvailable;
      });
  }

  setSelectedLevel(levelPath: string) {
    const level = getLevelFromPath(this.levelRoot!, levelPath);
    if (level != this.currentLevel) {
      this.onLevelSelected(level);
    }
  }

  onLevelSelected(level: LevelInfo) {
    this.selected.emit(level);
    this.currentLevel = level;
  }
}

export enum DisplayState {
  NoRootLevelSelected,
  LoadingLevelRoot,
  ErrorLoadingLevelRoot,
  LevelRootNotAvailable,
  LevelRootLoaded
}
