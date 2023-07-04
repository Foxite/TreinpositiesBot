import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
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
  }

  ngOnInit(): void {
    this.refreshUser(this.security.currentUser());
    this.security.userObservable().subscribe((user) => this.refreshUser(user));
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
