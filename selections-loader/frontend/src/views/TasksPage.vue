<template>
  <div>
    <v-container>
      <v-row>
        <v-col>
          <v-simple-table dense style="height: 80vh; overflow-y: auto">
            <template v-slot:default>
              <thead>
              <tr>
                <th class="text-left">Задание</th>
                <th class="text-left">Завершено?</th>
                <th class="text-left">Дата создания</th>
                <th class="text-left">Дата завершения</th>
                <th class="text-left">Прогресс</th>
              </tr>
              </thead>
              <tbody>
              <tr v-for="task in tasks" :key="task.id">
                <td class="text-left">{{ task.name }}</td>
                <td class="text-left">{{ formatLogical(task.isFinished) }}</td>
                <td class="text-left">{{ formatDate(task.createDate) }}</td>
                <td class="text-left">{{ formatDate(task.finishDate) }}</td>
                <td class="text-left">{{ formatPercentage(task) }}</td>
              </tr>
              </tbody>
            </template>
          </v-simple-table>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script lang="ts">
import {defineComponent, onBeforeMount, onUnmounted, ref} from '@vue/composition-api';
import Loading from '@/components/Loading.vue';
import {Task} from '@/models/Task';
import {TaskType} from '@/models/TaskType';
import {format} from 'date-fns';
import {Container} from '@/Container';
import {Store} from '@/store';

export default defineComponent({
  name: 'TasksPage',
  components: {Loading},
  setup: () => {
    const tasks = ref(new Array<Task>());
    let timerId = 0;

    const getTasks = async () => {
      const response = await fetch('/api/tasks');
      const json = await response.json() as [];

      const taskArray = new Array<Task>();

      json.forEach((j: any) => {
        taskArray.push(new Task({
          name: j.name,
          taskType: j.taskType,
          id: j.id,
          priority: j.priority,
          createDate: new Date(j.createDate),
          finishDate: j.finishDate == null ? null : new Date(j.finishDate),
          isFinished: j.isFinished,
          taskObjectId: j.taskObjectId,
          progress: j.progress,
        }));
      });

      tasks.value = taskArray;
    }

    onBeforeMount(async () => {
      await getTasks();

      timerId = setInterval(() => {
        getTasks();
      }, 1000);
    });

    onUnmounted(() => {
      clearInterval(timerId);
    });

    const formatLogical = (logical: boolean) => {
      return logical ? 'Да' : 'Нет';
    };

    const formatDate = (date: Date | null) => {
      if (date == null) {
        return '-';
      }

      return format(date, 'HH:mm dd.MM.yyyy');
    };

    const formatPercentage = (task: Task): string => {
      if (task.progress == null || task.isFinished){
        return '';
      }

      return `${task.progress}%`;
    };

    return {
      tasks,
      formatLogical,
      formatDate,
      formatPercentage,
    };
  },
});
</script>

<style lang="stylus" scoped>

</style>
