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
  @Input() rootLevelId: string | null = null;
  @Output() selected = new EventEmitter<LevelInfo | null>();

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
      if (this.rootLevelId !== null) {
        this.updateLevels(null, this.rootLevelId);
      }
    });

    /*
    this.route.paramMap.subscribe(paramMap => {
      this.updateGuild();
    });
     */
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes["rootLevelId"]) {
      this.updateLevels(changes["rootLevelId"].previousValue, changes["rootLevelId"].currentValue);
    }
  }

  updateLevels(oldRootId: string | null, newRootId: string) {
    // TODO use component input parameter
    const currentUser = this.security.currentUser;
    if (!currentUser) {
      return;
    }

    if (oldRootId == newRootId) {
      return;
    }

    if (newRootId === undefined || newRootId === null) { // todo which one is it
      console.log("this.rootLevelId is", newRootId);
      this.levelRoot = null;
      return;
    }

    this.rootLevel = currentUser.rootLevels[newRootId!];
    this.displayState = DisplayState.LoadingLevelRoot;

    // TODO set this.levelRoot
    this.levelsService.getLevelTree(newRootId)
      .then(levelRoot => {
        if (this.rootLevelId !== newRootId) {
          // this.guildId was changed before this promise resolved.
          return;
        }
        this.levelRoot = levelRoot;
        if (this.levelRoot === null) {
          this.displayState = DisplayState.LevelRootNotAvailable;
        } else {
          this.displayState = DisplayState.LevelRootLoaded;
        }
      })
      .catch(error => {
        console.error(error);
        this.displayState = DisplayState.LevelRootNotAvailable;
      });
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
