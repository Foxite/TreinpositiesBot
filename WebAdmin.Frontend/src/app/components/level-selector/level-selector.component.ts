import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {LevelInfo, RootLevelInfo} from "../../models/models";
import {SecurityService} from "../../services/security/security.service";
import {DiscordService} from "../../services/discord/discord.service";
import {ActivatedRoute, Router} from "@angular/router";

@Component({
  selector: 'app-level-selector',
  templateUrl: './level-selector.component.html',
  styleUrls: ['./level-selector.component.scss']
})
export class LevelSelectorComponent implements OnInit {
  @Input() rootLevelId: string | null = null;
  @Output() currentLevel = new EventEmitter<LevelInfo | null>();

  rootLevel: RootLevelInfo | null = null;
  levelRoot: LevelInfo | null = null; // these represent the same thing, but rootLevel resolves immediately, while levelRoot contains more info and is slow to retrieve
  displayState: DisplayState = DisplayState.NoGuildSelected;

  protected readonly JSON = JSON;
  protected readonly DisplayState = DisplayState;

  constructor(private security: SecurityService,
              private discord: DiscordService,
              private router: Router,
              private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.security.userObservable.subscribe(user => {
      this.updateGuild();
    });

    this.route.paramMap.subscribe(paramMap => {
      this.updateGuild();
    });
  }

  updateGuild() {
    // TODO use component input parameter
    const newGuildId = this.route.snapshot.params["guildId"];

    const currentUser = this.security.currentUser;
    if (!currentUser) {
      return;
    }

    if (newGuildId == this.rootLevelId) {
      return;
    }

    this.rootLevelId = newGuildId;
    if (this.rootLevelId === undefined || this.rootLevelId === null) { // todo which one is it
      this.levelRoot = null;
      return;
    }

    this.rootLevel = currentUser.rootLevels[this.rootLevelId!];
    this.displayState = DisplayState.LoadingGuild;

    this.ccs.getGuild(this.rootLevelId)
      .then(gc => {
        if (this.rootLevelId != newGuildId) {
          // this.guildId was changed before this promise resolved.
          return;
        }
        this.levelRoot = gc;
        if (this.levelRoot === null) {
          this.displayState = DisplayState.BotNotInGuild;
        } else {
          this.displayState = DisplayState.GuildSelected;
        }
      })
      .catch(error => {
        console.error(error);
        this.displayState = DisplayState.BotNotInGuild;
      });
  }
}

export enum DisplayState {
  NoGuildSelected,
  LoadingGuild,
  ErrorLoadingGuild,
  BotNotInGuild,
  GuildSelected
}
