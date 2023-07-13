import { NgModule } from '@angular/core';
import {RouterModule, Routes, UrlMatchResult, UrlSegment} from '@angular/router';
import {GuildsComponent} from "./components/guilds/guilds.component";

const routes: Routes = [
  {
    // This must be a single route, because otherwise, Angular recreates the component when you go to a different route.
    // That causes the guild data to be retrieved again, and the page will disappear while it's loading.
    path: 'guilds/:guildId/:channelId',
    component: GuildsComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
