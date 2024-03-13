import {reactive} from '@vue/composition-api';
import {State} from '@/store/state';

export class Store {
    get state(): { reactive: State } {
        return this._state;
    }
    private readonly _state!: {reactive: State};

    constructor() {
        this._state = reactive({
            reactive: new State({
                isAuthenticated: false,
            }),
        });
    }

    public async initState(): Promise<any> {
        let response = await fetch('/api/auth/is-authenticated');
        if (response.ok) {
            this._state.reactive.isAuthenticated = true;
            response = await fetch('/api/initialize');
            const needInitialization = await response.json();
            if (needInitialization) {
                response = await fetch('/api/initialize', {
                    method: 'POST',
                    headers: {
                        Accept: 'application/json',
                        'Content-Type': 'application/json',
                    },
                });
            }
            await this._state.reactive.startJobs();
        }
    }
}


