import { Component, input, InputSignal } from '@angular/core';

@Component({
  selector: 'gif-list-item',
  imports: [],
  templateUrl: './gif-list-item.component.html',
})
export class GifListItemComponent {
  public imageUrl: InputSignal<string> = input.required<string>();
}
