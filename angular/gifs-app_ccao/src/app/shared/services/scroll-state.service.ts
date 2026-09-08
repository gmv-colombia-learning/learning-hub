import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ScrollStateService {
  public trendingScrollState = signal(0);
}
