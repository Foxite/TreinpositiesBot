import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {MainComponent} from "./components/main/main.component";
import {GuildsComponent} from "./components/guilds/guilds.component";

const routes: Routes = [
  {path: '', component: MainComponent},
  {path: 'guilds', component: GuildsComponent},
  {path: 'guilds/:guildId', component: GuildsComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
