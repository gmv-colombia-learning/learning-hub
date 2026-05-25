import { Component, input, InputSignal } from '@angular/core';
import { GifListItemComponent } from './gif-list-item/gif-list-item.component';
import { Gif } from '../../models/gif';

@Component({
  selector: 'gif-list',
  imports: [GifListItemComponent],
  templateUrl: './gif-list.component.html',
})
export class GifListComponent {
  public gifs: InputSignal<Gif[]> = input.required<Gif[]>();
}
