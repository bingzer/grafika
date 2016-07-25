"use strict";
var synchronizer_1 = require("../libs/synchronizer");
function sync(req, res, next) {
    var userId = req.user._id;
    var localSync = req.body;
    if (!localSync)
        return next(400);
    localSync._id = userId;
    if (!localSync.animations || !localSync.dateModified || !localSync.clientId)
        return next(400);
    var synchronizer = new synchronizer_1.Synchronizer(localSync);
    synchronizer
        .sync()
        .then(function (syncResult) {
        res.send(syncResult);
    })
        .catch(function (err) {
        next(err);
    });
}
exports.sync = sync;
//# sourceMappingURL=sync.js.map