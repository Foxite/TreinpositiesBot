import {Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges} from '@angular/core';
import {LevelInfo, RootLevelInfo} from "../../models/models";
import {SecurityService} from "../../services/security/security.service";
import {LevelsService} from "../../services/levels/levels.service";

@Component({
  selector: 'app-level-selector',
  templateUrl: './level-selector.component.html',
  styleUrls: ['./level-selector.component.scss']
})
export class LevelSelectorComponent implements OnInit, OnChanges {
  @Input() levelPath: string | null = null;
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

  updateLevels(oldLevelPath: string | null, newLevelPath: string) {
    // TODO use component input parameter
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

    // TODO set this.levelRoot
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
    const levelPathSplit = levelPath.split(':');

    if (levelPathSplit[0] !== this.levelRoot?.id) {
      throw new Error("Attempting to set selected level with mismatched root");
    }

    let first = true;
    let level = this.levelRoot;
    for (const pathSegment of levelPathSplit) {
      if (first) {
        first = false;
        continue;
      }

      if (!level.children) {
        throw new Error("Level path is not found in tree");
      }

      level = level.children[pathSegment];
      if (!level) {
        throw new Error("Level path is not found in tree");
      }
    }

    this.currentLevel = level;
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
