import {Component, OnInit} from '@angular/core';
import {LevelInfo} from "../../models/models";
import {ActivatedRoute, Router} from "@angular/router";

@Component({
  selector: 'app-config-panel',
  templateUrl: './config-panel.component.html',
  styleUrls: ['./config-panel.component.scss']
})
export class ConfigPanelComponent implements OnInit {
  currentRootLevelId: string | null = null;
  currentLevelPath: string | null = null;
  currentLevel: LevelInfo | null = null;
  highlightedPath: string | null = null;

  constructor(private router: Router,
              private activatedRoute: ActivatedRoute) {
  }

  ngOnInit() {
    const setLevelToPath = () => {
      const levelPath = this.activatedRoute.snapshot.url[0].parameters['level'];
      if (levelPath) {
        this.currentLevelPath = levelPath;

        const levelPathSplit = levelPath.split(':');
        this.currentRootLevelId = levelPathSplit[0];
      }
    }

    /*
    this.router.events.subscribe(evt => {
      if (evt.type == EventType.NavigationEnd) {
        setLevelToPath();
      }
    });*/

    setLevelToPath();
  }

  onSelectRootLevel(rootLevelId: string) {
    this.currentRootLevelId = rootLevelId;
    this.currentLevelPath = rootLevelId;
    this.router.navigate(['config', { level: rootLevelId }]);
  }

  onSelectLevel(level: LevelInfo) {
    let levelPath = level.id;

    let pathCurrentLevel = level;
    while (pathCurrentLevel.parent) {
      pathCurrentLevel = pathCurrentLevel.parent;
      levelPath = `${pathCurrentLevel.id}:${levelPath}`;
    }

    this.router.navigate(['config', { level: levelPath }]);

    this.currentLevelPath = levelPath;

    this.currentLevel = level;
  }

  onHoverFormElement(specifiedAtPath: string) {
    this.highlightedPath = specifiedAtPath;
  }
}
