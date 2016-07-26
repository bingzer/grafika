module GrafikaApp {
    export class AuthService extends BaseService {
        private user: User;
        
        public static $inject = ['appCommon', '$rootScope', 'apiService', 'jwtHelper'];
        constructor (
            appCommon: AppCommon,
            private $rootScope: ng.IRootScopeService,
            private apiService: ApiService,
            private jwtHelper: ng.jwt.IJwtHelper
        ){
            super(appCommon);
        }

        register(user: any): ng.IPromise<any> {
            return this.apiService.post('accounts/register', user);
        }
        
        login(user: any, provider: string): ng.IPromise<any> {
            if (!provider)
                return this.apiService.post('accounts', user).success(() => this.authenticate());
            else {
                window.location.href = this.appCommon.appConfig.apiBaseUrl + 'accounts/' + provider.toLowerCase();
                return this.appCommon.$q.when(true);
            }                
        }

        setAccessToken(token: string) {
            if (!token) return this.appCommon.$q.when(false);
            this.user = null;
            this.appCommon.$window.sessionStorage.setItem('token', token);
            this.user = this.getUser();
            return this.appCommon.$q.when(true);
        }
        
        getAccessToken(): string{
            return this.appCommon.$window.sessionStorage.getItem('token');
        }

        logout(): ng.IPromise<any> {
            return this.apiService.post('accounts/logout').finally(() => {
                this.clearToken();
                this.clearSession();
                return this.authenticate(true).then(() => {
                    this.appCommon.navigateHome()
                    return this.appCommon.$q.when(true); 
                });
            });
        }

        clearToken() {
            this.appCommon.$window.sessionStorage.removeItem('token');
        }
        
        clearSession() {
            this.appCommon.$cookieStore.remove('stdx.session');
        }
        
        authenticate(skipLogout?: boolean): ng.IPromise<any>{
            var deferred = this.appCommon.$q.defer();
            this.apiService.post('accounts/authenticate')
                    .success((res) => { 
                        this.setAccessToken(res.token);
                        deferred.resolve();
                    })
                    .error(() => {
                        deferred.reject();
                        if (!skipLogout) this.logout();
                    });
            return deferred.promise;
        }

        isAuthenticated(): boolean {
            return this.appCommon.$window.sessionStorage.getItem('token') != null;
        }
        
        isAuthorized(roles: string | [string]) {
            if (!this.isAuthenticated()) 
                return false;
            var explicit = angular.isDefined(roles);
            var user = this.getUser();
            if (!explicit){
                return user.hasRoles('user');
                // var route = this.appCommon.getCurrentRoute();
                // if (route) 
                //     roles = route.config.roles; 
            }
            else{
                return user.hasRoles(roles);
            }
        }
        
        changePassword(pwd): ng.IPromise<any>{
            return this.apiService.post('accounts/pwd', pwd);
        }
        
        resetPassword(user): ng.IPromise<any>{
            return this.apiService.post('accounts/pwd/reset', user);
        }
        
        getDisqusToken(): ng.IPromise<any>{
            return this.apiService.get('accounts/disqus');
        }

        getUser(): User {
            if (!this.appCommon.$window.sessionStorage.getItem('token')) {
                this.user = null;
                return this.user;
            }
            if (this.user) return this.user;
            
            var payload: any = this.jwtHelper.decodeToken(this.appCommon.$window.sessionStorage.getItem('token'));
            var user = new User(payload);
            return user;
        }

    }
}