import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable, of, switchMap, tap } from 'rxjs';

import { environment } from 'src/environments/environment';
import { User } from '@auth/interfaces/user.interface';
import {
  Options,
  Gender,
  Product,
  ProductsResponse,
} from '@products/interfaces/product.interface';
import { mockProducts } from '@products/mocks/mock-products';

const baseUrl = environment.baseUrl;

const emptyProduct: Product = {
  id: 'new',
  title: '',
  price: 0,
  description: '',
  slug: '',
  stock: 0,
  sizes: [],
  gender: Gender.Men,
  tags: [],
  images: [],
  user: {} as User,
};

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private http = inject(HttpClient);

  private productsCache = new Map<string, ProductsResponse>();
  private productCache = new Map<string, Product>();

  getProducts(options: Options): Observable<ProductsResponse> {
    const { limit = 9, offset = 0, gender = '' } = options;

    if (environment.useMockApi) {
      const products = gender
        ? mockProducts.filter((product) => product.gender === gender)
        : mockProducts;
      const paginatedProducts = products.slice(offset, offset + limit);

      return of({
        count: products.length,
        pages: Math.ceil(products.length / limit),
        products: paginatedProducts,
      });
    }

    const key = `${limit}-${offset}-${gender}`; // 9-0-''

    if (this.productsCache.has(key)) {
      return of(this.productsCache.get(key)!);
    }

    return this.http
      .get<ProductsResponse>(`${baseUrl}/products`, {
        params: {
          limit,
          offset,
          gender,
        },
      })
      .pipe(tap((resp) => this.productsCache.set(key, resp)));
  }

  getProductByIdSlug(idSlug: string): Observable<Product> {
    if (environment.useMockApi) {
      const product = mockProducts.find(
        ({ id, slug }) => id === idSlug || slug === idSlug,
      );

      if (!product) {
        throw new Error(`Mock product not found: ${idSlug}`);
      }

      return of(product);
    }

    if (this.productCache.has(idSlug)) {
      return of(this.productCache.get(idSlug)!);
    }

    return this.http
      .get<Product>(`${baseUrl}/products/${idSlug}`)
      .pipe(tap((product) => this.productCache.set(idSlug, product)));
  }

  getProductById(id: string): Observable<Product> {
    if (id === 'new') {
      return of(emptyProduct);
    }

    if (environment.useMockApi) {
      const product = mockProducts.find(
        ({ id: productId }) => productId === id,
      );

      if (!product) {
        throw new Error(`Mock product not found: ${id}`);
      }

      return of(product);
    }

    if (this.productCache.has(id)) {
      return of(this.productCache.get(id)!);
    }

    return this.http
      .get<Product>(`${baseUrl}/products/${id}`)
      .pipe(tap((product) => this.productCache.set(id, product)));
  }

  updateProduct(
    id: string,
    productLike: Partial<Product>,
    imageFileList?: FileList,
  ): Observable<Product> {
    const currentImages = productLike.images ?? [];

    if (environment.useMockApi) {
      const uploadedMockImages = imageFileList
        ? Array.from(imageFileList).map((imageFile) =>
            URL.createObjectURL(imageFile),
          )
        : [];

      const productIndex = mockProducts.findIndex(
        ({ id: productId }) => productId === id,
      );

      if (productIndex === -1) {
        throw new Error(`Mock product not found: ${id}`);
      }

      const updatedProduct: Product = {
        ...mockProducts[productIndex],
        ...productLike,
        images: [...currentImages, ...uploadedMockImages],
        id,
      };

      mockProducts[productIndex] = updatedProduct;
      this.updateProductCache(updatedProduct);

      return of(updatedProduct);
    }

    return this.uploadImages(imageFileList).pipe(
      map((imageNames) => ({
        ...productLike,
        images: [...currentImages, ...imageNames],
      })),
      switchMap((updatedProduct) =>
        this.http.patch<Product>(`${baseUrl}/products/${id}`, updatedProduct),
      ),
      tap((product) => this.updateProductCache(product)),
    );
  }

  createProduct(
    productLike: Partial<Product>,
    imageFileList?: FileList,
  ): Observable<Product> {
    const currentImages = productLike.images ?? [];

    if (environment.useMockApi) {
      const uploadedMockImages = imageFileList
        ? Array.from(imageFileList).map((imageFile) =>
            URL.createObjectURL(imageFile),
          )
        : [];

      const createdProduct: Product = {
        ...emptyProduct,
        ...productLike,
        images: [...currentImages, ...uploadedMockImages],
        id: this.buildMockProductId(),
        slug: productLike.slug ?? this.buildSlugFromTitle(productLike.title),
      };

      mockProducts.unshift(createdProduct);
      this.updateProductCache(createdProduct);

      return of(createdProduct);
    }

    return this.uploadImages(imageFileList).pipe(
      map((imageNames) => ({
        ...productLike,
        images: [...currentImages, ...imageNames],
      })),
      switchMap((updatedProduct) =>
        this.http.post<Product>(`${baseUrl}/products`, updatedProduct),
      ),
      tap((product) => this.updateProductCache(product)),
    );
  }

  updateProductCache(product: Product) {
    const productId = product.id;

    this.productCache.set(productId, product);

    this.productsCache.forEach((productResponse) => {
      productResponse.products = productResponse.products.map(
        (currentProduct) =>
          currentProduct.id === productId ? product : currentProduct,
      );
    });
  }

  uploadImages(images?: FileList): Observable<string[]> {
    if (!images) {
      return of([]);
    }

    const uploadObservables = Array.from(images).map((imageFile) =>
      this.uploadImage(imageFile),
    );

    return forkJoin(uploadObservables).pipe(
      tap((imageNames) => console.log({ imageNames })),
    );
  }

  uploadImage(imageFile: File): Observable<string> {
    if (environment.useMockApi) {
      return of(URL.createObjectURL(imageFile));
    }

    const formData = new FormData();
    formData.append('file', imageFile);

    return this.http
      .post<{ fileName: string }>(`${baseUrl}/files/product`, formData)
      .pipe(map((resp) => resp.fileName));
  }

  private buildMockProductId() {
    return `mock-${Date.now()}`;
  }

  private buildSlugFromTitle(title?: string) {
    const baseTitle = title?.trim() || 'new-product';

    return baseTitle
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
  }
}
