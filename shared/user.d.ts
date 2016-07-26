declare namespace Grafika {    
    interface IUser {
        _id: string | any;
        email: string;
        username: string;
        firstName: string;
        lastName: string;
        dateCreated: number;
        dateModified: number;
        active: boolean;
        roles: string[];

        prefs: IUserPreference;
    }

    interface IUserPreference {
        avatar: string;
        backdrop: string;
        drawingIsPublic: boolean,
        drawingAuthor: string;
        drawingTimer: number;
        playbackLoop: boolean;   
    }
}