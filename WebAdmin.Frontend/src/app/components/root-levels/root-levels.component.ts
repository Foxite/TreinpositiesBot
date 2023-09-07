import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {ChannelConfigService} from "../../services/channel-config.service";
import {SecurityService} from "../../services/security/security.service";
import {ActivatedRoute, EventType, Router} from "@angular/router";
import {User} from "../../services/security/user";
import {RootLevelInfo} from "../../models/models";

@Component({
  selector: 'app-root-levels',
  templateUrl: './root-levels.component.html',
  styleUrls: ['./root-levels.component.scss']
})
export class RootLevelsComponent implements OnInit {
  rootLevels!: RootLevelInfo[];
  currentRootLevelId!: string | null;

  @Output() selected = new EventEmitter<string>();

  constructor(private ccs: ChannelConfigService,
              private security: SecurityService,
              private router: Router,
              private activatedRoute: ActivatedRoute) {
  }

  ngOnInit(): void {
    this.router.events.subscribe(evt => {
      if (evt.type == EventType.NavigationEnd) {
        this.currentRootLevelId = this.activatedRoute.snapshot.url[0].parameters['level']
        this.selected.emit(this.currentRootLevelId);
      }
    });

    this.currentRootLevelId = this.activatedRoute.snapshot.url[0].parameters['level']
    this.selected.emit(this.currentRootLevelId);

    const currentUser = this.security.currentUser;
    if (currentUser) {
      this.updateRootLevels(currentUser);
    }

    this.security.userObservable.subscribe(user => {
      if (user) {
        this.updateRootLevels(user);
      }
    });
  }

  updateRootLevels(user: User) {
    this.rootLevels = Object.values(user.rootLevels);
  }
}
