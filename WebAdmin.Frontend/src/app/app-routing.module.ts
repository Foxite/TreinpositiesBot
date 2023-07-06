import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {GuildsComponent} from "./components/guilds/guilds.component";

const routes: Routes = [
  {path: '', component: GuildsComponent},
  {path: 'guilds', component: GuildsComponent},
  {path: 'guilds/:guildId', component: GuildsComponent},
  {path: 'guilds/:guildId/:channelId', component: GuildsComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
