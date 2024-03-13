import { inject, provide } from '@vue/composition-api';
import { Store } from '@/store';

// eslint-disable-next-line @typescript-eslint/no-namespace
export namespace Container {
  // eslint-disable-next-line no-inner-declarations
  export function setup() {
    provide<Store>(nameof(Store), new Store());
  }
  // eslint-disable-next-line no-inner-declarations
  export function resolve<T>(key: string) : T {
    return inject<T>(key) as T;
  }
}
