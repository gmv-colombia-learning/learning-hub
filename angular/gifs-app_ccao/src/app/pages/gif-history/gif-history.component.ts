import { Component, computed, inject, Signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

import { GifService } from '../../shared/services/gifs.service';
import { GifListComponent } from '../../shared/components/gif-list/gif-list.component';
import { Gif } from '../../shared/models/gif';

@Component({
  selector: 'app-gif-history',
  imports: [GifListComponent],
  templateUrl: './gif-history.component.html',
})
export default class GifHistoryComponent {
  private gifService = inject(GifService);

  public query: Signal<string> = toSignal(
    //En vez de usar subscribe, se puede usar toSignal para convertir el observable en una señal reactiva
    inject(ActivatedRoute).params.pipe(map((params) => params['query'])),
  );

  public gifsByKey: Signal<Gif[]> = computed(() =>
    this.gifService.getHistoryGifs(this.query()),
  );
}
