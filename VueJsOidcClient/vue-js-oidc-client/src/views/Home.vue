<template>
    <div class="home">
        <img alt="Vue logo" src="../assets/logo.png">
        <div class="home">
            <p v-if="isLoggedIn">User: {{ username }}</p>
            <button @click="login" v-if="!isLoggedIn">Login</button>
            <button @click="logout" v-if="isLoggedIn">Logout</button>
        </div>
        <HelloWorld v-if="isLoggedIn" msg="OIDC Vue.js" />
    </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import HelloWorld from '@/components/HelloWorld.vue'; // @ is an alias to /src
import AuthService from '@/services/auth.service';

import axios from 'axios';

const auth = new AuthService();

@Component({
  components: {
    HelloWorld,
  },
})

export default class Home extends Vue {
    public currentUser: string = '';
    public accessTokenExpired: boolean | undefined = false;
    public isLoggedIn: boolean = false;

    public todos: [] = [];

    get username(): string {
        return this.currentUser;
    }

    public login() {
        auth.login();
    }

    public logout() {
        auth.logout();
    }

    public mounted() {
        auth.getUser().then((user) => {
            this.currentUser = user.profile.name;
            this.accessTokenExpired = user.expired;

            this.isLoggedIn = (user !== null && !user.expired);
        });
    }

    public getProtectedApiData() {
        axios.get('http://jsonplaceholder.typicode.com/todos')
            .then((response: any) => {
                this.todos = response.data;
            })
            .catch((error: any) => {
                alert(error);
            });
    }
}
</script>
