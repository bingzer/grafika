import * as mongoose from 'mongoose';
import * as config from '../configs/config';
import * as q from 'q';

import restful = require('../libs/restful');

const bcrypt      = require('bcrypt-nodejs');
const crypto      = require('crypto-js');
const jwt         = require('jsonwebtoken');
const SECRET      = config.setting.$server.$superSecret;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

export interface IUser extends mongoose.Document, Grafika.IUser {
    activation: IActivation;

    local: ILocal;
    facebook: IFacebook;
    google: IGoogle;

    generateHash(password: string): string;
    generateActivationHash(): string;
    validPassword(password: string): boolean;
    validActivationHash(activationHash : string): boolean;
    validActivationTimestamp(): boolean;
    sanitize(): IUser;
}

export interface IActivation {
    hash: string;
    timestamp: Date;
}

export interface IAccount {
    id: string;
}

export interface ILocal extends IAccount {
    password: string,
    registered: boolean;
}

export interface IFacebook extends IAccount {
    token: string;
    displayName: string;
}

export interface IGoogle extends IAccount {
    token: string;
    displayName: string;
}

export const UserSchema = new mongoose.Schema({
    firstName           : String,
    lastName            : String,
    username            : { type: String, lowercase: true, trim: true, required: true, default: randomUsername() },
    email               : { type: String, lowercase: true, trim: true, required: true },
    dateCreated         : { type: Number, required: true },
    dateModified        : { type: Number, required: true },
    active              : { type: Boolean, default: false },
    roles               : { type: [String], default: ['user'] },
    local               : {
        password        : String,
        registered      : Boolean
    },
    facebook            : {
        id              : String,
        token           : String,
        displayName     : String
    },
    google              : {
        id              : String,
        token           : String,
        displayName     : String
    },
    activation          : {
        hash            : String,
        timestamp       : Date
    },
    prefs               : {
        avatar          : { type: String, default: '/assets/img/ic_user.png' },
        backdrop        : { type: String, default: '/assets/img/ic_backdrop.png' },
        drawingIsPublic : { type: Boolean, default: false },
        drawingAuthor   : { type: String },
        drawingTimer    : { type: Number, default: 1000 },
        playbackLoop    : { type: Boolean, default: false }
    }
});

// methods ======================
// generating a hash
UserSchema.methods.generateHash = function(password) : string {
    return bcrypt.hashSync(password, bcrypt.genSaltSync(8), null);
};
// generate random active hash
UserSchema.methods.generateActivationHash = function() : string{
    return UserSchema.methods.generateHash(crypto.lib.WordArray.random(128/8))
}
// checking if password is valid
UserSchema.methods.validPassword = function(password: string) : boolean {
    if (!this.local.password) return false;
    return bcrypt.compareSync(password, this.local.password);
};
UserSchema.methods.validActivationHash = function(activationHash: string) : boolean{
    return this.activation.hash === activationHash;
};
UserSchema.methods.validActivationTimestamp = function(): boolean{
    let expiredTime = 5 * 60 * 1000;
    if (this.activation.timestamp && Math.abs(Date.now() - this.activation.timestamp) < expiredTime)
        return true;
    return false;
};
UserSchema.methods.sanitize = function(): IUser {
    return sanitize(this);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////

let User = <restful.IModel<IUser>> restful.model('users', UserSchema);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////

export function generateJwtToken(user: IUser | any) : string {
    return jwt.sign(sanitize(user), SECRET, {
        expiresIn: '24hr' // expires in 24 hours
    });
}

export function sanitize(user: IUser | any) : any | IUser {
    let lean = user;
    if (user.toObject) {
        user = user.toObject();
    }
    delete user.local;
    delete user.facebook;
    delete user.google;
    delete user.activation;
    return user;
}

export function checkAvailability(user : IUser | any) : q.IPromise<{}> {
    let deferred = q.defer();
    let query = {
        username: user.username,
        email: { $ne: user.email }
    };
    User.findOne(query, (err, user) => {
        if (err || user) deferred.reject('failed');
        else deferred.resolve();
    });
    return deferred.promise;
}

export function userQuery(username: string) : any {
    let name = username ? username.toLowerCase() : null;
    return { $or:[{ 'email': name }, { 'username': name }] };
}

export function ensureAdminExists() : ng.IPromise<IUser> {
    let defer = q.defer<IUser>();

	User.findOne(userQuery('admin'), (err, user) => {
        if (err) return defer.reject(err);
        if (!user) {
			user = new User();
			user.firstName        = 'grafika';
			user.lastName         = 'admin';
			user.email            = 'grafika-admin@bingzer.com';
			user.dateCreated      = Date.now();
			user.dateModified     = Date.now();
			user.active           = true;
			user.local.registered = true;
			user.local.password   = user.generateHash('password');
            user.roles.push('administrator');
			user.save();
		}
        defer.resolve(user);
	});

    return defer.promise;
};

export function randomUsername(){
    return 'user-' + (("000000" + (Math.random()*Math.pow(36,6) << 0).toString(36)).slice(-6));
}

export { User };