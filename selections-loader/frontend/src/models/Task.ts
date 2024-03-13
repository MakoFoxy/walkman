import {ModelBase} from '@/models/ModelBase';
import {TaskType} from '@/models/TaskType';

export class Task extends ModelBase {
    public name!: string

    public isFinished!: boolean

    public priority!: number

    public createDate!: Date

    public finishDate: Date | null = null

    public taskObjectId!: string

    public taskType!: TaskType

    public progress: number | null = null

    constructor(init?: Partial<Task>) {
        super();
        Object.assign(this, init);
    }
}

