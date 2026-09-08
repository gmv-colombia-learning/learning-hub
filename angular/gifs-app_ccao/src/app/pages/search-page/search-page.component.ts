import { Component, inject, signal, WritableSignal } from '@angular/core';

import { GifListComponent } from '../../shared/components/gif-list/gif-list.component';
import { GifService } from '../../shared/services/gifs.service';
import { Gif } from '../../shared/models/gif';

@Component({
  selector: 'app-search-page',
  imports: [GifListComponent],
  templateUrl: './search-page.component.html',
})
export default class SearchPageComponent {
  private gifService = inject(GifService);
  public gifs: WritableSignal<Gif[]> = signal<Gif[]>([]);

  public onSearch(query: string): void {
    this.gifService.searchGifs(query).subscribe((resp) => {
      this.gifs.set(resp);
    });
  }
}
