export class State {
    public isAuthenticated = false;

    public startJobs(): Promise<Response> {
        return fetch('/api/tasks/start-background-jobs', {
            method: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
        });
    }

    constructor(init?: Partial<State>) {
        Object.assign(this, init);
    }
}
