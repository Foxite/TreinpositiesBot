import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {AppModule} from "../../app.module";
import {filter} from "rxjs";

@Component({
	selector: 'app-top-bar',
	templateUrl: './top-bar.component.html',
	styleUrls: ['./top-bar.component.scss']
})
export class TopBarComponent implements OnInit {
	constructor() {
	}

	ngOnInit(): void {
	}
}
