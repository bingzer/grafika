module GrafikaApp {
    export function KeyboardFilter($window: ng.IWindowService){
        return (str) => {
            if (!str) return;
            let keys = str.split('-');
            let isOSX = /Mac OS X/.test($window.navigator.userAgent);
            let seperator = (!isOSX || keys.length > 2) ? '+' : '';
            let abbreviations = {
                M: isOSX ? '⌘' : 'Ctrl',
                A: isOSX ? 'Option' : 'Alt',
                S: 'Shift'
            };
            return keys.map((key, index) => {
                let last = index == keys.length - 1;
                return last ? key : abbreviations[key];
            }).join(seperator);
        };
    }
}