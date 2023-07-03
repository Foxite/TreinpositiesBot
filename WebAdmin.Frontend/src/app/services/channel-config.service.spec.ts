import { TestBed } from '@angular/core/testing';

import { ChannelConfigService } from './channel-config.service';

describe('ChannelConfigService', () => {
  let service: ChannelConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ChannelConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
