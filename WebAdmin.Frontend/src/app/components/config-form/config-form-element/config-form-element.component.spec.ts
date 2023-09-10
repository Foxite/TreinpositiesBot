import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ConfigFormElementComponent } from './config-form-element.component';

describe('ConfigFormElementComponent', () => {
  let component: ConfigFormElementComponent;
  let fixture: ComponentFixture<ConfigFormElementComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ConfigFormElementComponent]
    });
    fixture = TestBed.createComponent(ConfigFormElementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
