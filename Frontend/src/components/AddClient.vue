<template>
    <div>
        <v-container fluid fill-height>
            <v-layout align-center justify-center>
                <v-flex xs12 sm12 md12>
                    <v-card class="elevation-12">
                        <v-toolbar dark color="primary">
                            <v-toolbar-title>Добавление клиента</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <v-form v-model="valid" lazy-validation ref="form">
                                <v-text-field :rules="requiredRule" name="name" label="Название клиента" type="text"
                                    v-model.trim="organization.name"></v-text-field>
                                <v-text-field :rules="requiredRule" name="name" label="БИН" type="text"
                                    v-model.trim="organization.bin"></v-text-field>
                                <v-text-field :rules="requiredRule" name="name" label="Юр Адрес" type="text"
                                    v-model.trim="organization.address"></v-text-field>
                                <v-text-field :rules="requiredRule" name="name" label="Банк" type="text"
                                    v-model.trim="organization.bank"></v-text-field>
                                <v-text-field :rules="requiredRule" name="name" label="ИИК" type="text"
                                    v-model.trim="organization.iik"></v-text-field>
                                <v-text-field :rules="requiredRule" name="name" label="Телефон" type="text"
                                    v-model.trim="organization.phone"></v-text-field>
                            </v-form>

                            <v-divider style="padding-top: 10px; padding-bootom: 10px;"></v-divider>

                            <v-flex xs12 sm12 md12>
                                <v-form lazy-validation v-for="(client, index) in organization.clients" v-bind:key="index">
                                    <v-flex xs10 sm10 md10>
                                        <h3>Клиент {{ index + 1 }}</h3>
                                    </v-flex>
                                    <v-flex xs2 sm2 md2>
                                        <v-btn @click="() => removeClient(index)">-</v-btn>
                                    </v-flex>
                                    <v-text-field :rules="requiredRule" name="firstName" label="Фамилия" type="text"
                                        v-model.trim="client.firstName"></v-text-field>
                                    <v-text-field :rules="requiredRule" name="secondName" label="Имя" type="text"
                                        v-model.trim="client.secondName"></v-text-field>
                                    <v-text-field name="lastName" label="Отчество" type="text"
                                        v-model.trim="client.lastName"></v-text-field>
                                    <v-text-field :rules="requiredRule" name="Email" label="Email" type="email"
                                        v-model.trim="client.email"></v-text-field>
                                    <v-text-field :rules="requiredRule" name="phoneNumber" label="Телефон" type="text"
                                        v-model.trim="client.phoneNumber"></v-text-field>
                                    <v-combobox :items="roles" label="Выберите роль"
                                        v-model="client.role" return-object item-value="id" item-text="name" :rules="requiredRule"></v-combobox>
                                    <v-combobox :items="objects" label="Выберите объекты"
                                        v-model="client.objects" return-object item-value="id" item-text="name" multiple></v-combobox>
                                    <v-text-field :rules="requiredRule" name="password" label="Пароль" type="password"
                                        v-model.trim="client.password"></v-text-field>
                                </v-form>
                            </v-flex>
                            <v-flex xs12 sm12 md12>
                                <v-btn @click="addClient">Добавить клиента</v-btn>
                            </v-flex>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer></v-spacer>
                            <v-btn color="primary" @click="addClientButtonClick" :disabled="loadingInProgress">Добавить</v-btn>
                            <v-btn color="primary" @click="$router.push('/home#clients')">Отмена</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-flex>
            </v-layout>
        </v-container>

        <div class="text-xs-center">
            <v-dialog v-model="loadingInProgress" hide-overlay persistent width="300">
                <v-card color="primary" dark>
                    <v-card-text>
                        Идет загрузка, пожалуйста ожидайте
                        <v-progress-linear indeterminate color="white" class="mb-0"></v-progress-linear>
                    </v-card-text>
                </v-card>
            </v-dialog>
        </div>
    </div>
</template>

<script>
    export default {
        name: 'add-client',
        data: () => ({
            valid: true,
            requiredRule: [
                v => !!v || 'Поле обязательно для заполнения'
            ],
            loadingInProgress: false,
            organization: {
                name: '',
                bin: '',
                address: '',
                bank: null,
                iik: null,
                phone: null,
                clients: []
            },
            roles: [],
            objects: []
        }),
        async beforeMount() {
            const roleResponse = await this.axios.get(`roles?filter=Client`);
            this.roles = roleResponse.data;
            const objectsResponse = await this.axios.get(`object/all`);
            this.objects = objectsResponse.data;
        },
        methods: {
            async addClientButtonClick() {
                if (!this.$refs.form.validate()) {
                    return
                }

                this.loadingInProgress = true

                try {
                    await this.axios.post('organizations', this.organization)
                } catch (err) {
                    this.loadingInProgress = false
                    return
                }

                this.$router.push('/home#clients')
            },
            addClient() {
                this.organization.clients.push({
                    firstName: '',
                    secondName: '',
                    lastName: '',
                    email: '',
                    phoneNumber: '',
                    role: null,
                    password: ''
                })
            },
            removeClient(index) {
                const clients = [];

                for (let i = 0; i < this.organization.clients.length; i++) {
                    if (i === index){
                        continue;
                    }
                    clients.push(this.organization.clients[i]);
                }

                this.organization.clients = clients;
            }
        }
    }
</script>