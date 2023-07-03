import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GuildsComponent } from './guilds.component';

describe('GuildsComponent', () => {
  let component: GuildsComponent;
  let fixture: ComponentFixture<GuildsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [GuildsComponent]
    });
    fixture = TestBed.createComponent(GuildsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
