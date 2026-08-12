import { Pipe, PipeTransform } from '@angular/core';
import { environment } from 'src/environments/environment';

const baseUrl = environment.baseUrl;

@Pipe({
  name: 'productImage',
})
export class ProductImagePipe implements PipeTransform {
  transform(value: string | string[]): string {
    const resolveImage = (image: string): string => {
      if (
        image.startsWith('http') ||
        image.startsWith('./assets') ||
        image.startsWith('/assets') ||
        image.startsWith('blob:') ||
        image.startsWith('data:')
      ) {
        return image;
      }

      return `${baseUrl}/files/product/${image}`;
    };

    if (typeof value === 'string') {
      return resolveImage(value);
    }

    const image = value.at(0);

    if (!image) {
      return './assets/images/no-image.jpg';
    }

    return resolveImage(image);
  }
}
