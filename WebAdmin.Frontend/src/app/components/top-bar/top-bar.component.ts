import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {AppModule} from "../../app.module";
import {filter} from "rxjs";
import {SecurityService} from "../../services/security/security.service";
import {User} from "../../services/security/user";

@Component({
	selector: 'app-top-bar',
	templateUrl: './top-bar.component.html',
	styleUrls: ['./top-bar.component.scss']
})
export class TopBarComponent implements OnInit {
  @Input()  username!: string | "";
  @Output() usernameChange = new EventEmitter<string>();

  constructor(public security: SecurityService) {
    this.refreshUser(security.currentUser());

    security.userUpdated().subscribe(user => {
      this.refreshUser(user);
    })
  }

  ngOnInit(): void {
  }

  private refreshUser(user: User | null) {
    if (user) {
      this.username = user.name;
    } else {
      this.username = "";
    }
    this.usernameChange.emit(this.username);
  }
}
