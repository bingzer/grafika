module GrafikaApp {
    export class AnimationDrawingController extends BaseAnimationController {
        grafika: Grafika.IGrafika = new Grafika();
        
        public static $inject = ['appCommon', 'authService', 'animationService', 'frameService', 'resourceService'];
        constructor(
            appCommon: AppCommon, 
            authService: AuthService,
            protected animationService: AnimationService,
            protected frameService: FrameService,
            protected resourceService: ResourceService
        ){
            super(appCommon, authService, animationService, frameService, resourceService);
        }

        onLoaded(animation: Grafika.IAnimation) {
            this.appCommon.elem('#canvas-container').css('width', this.animation.width).css('height', this.animation.height);
            this.grafika.initialize('#canvas', { drawingMode: 'paint' }, this.animation);
            this.frameService.get(this.grafika.getAnimation()).then((res) => {
                this.grafika.setFrames(res.data);
            })
        }

        showProperties(ev: MouseEvent) {
            return this.appCommon.showDialog('AnimationEditController', '/app/animation/edit.html', ev).then(() => this.load());
        }

		save(exit: boolean) {
			this.grafika.save();

            var animation = this.grafika.getAnimation();
			this.animationService.update(animation).then((res) => {
                return this.resourceService.saveThumbnail(animation);
            }).then((res) => {
                return this.resourceService.upload(res.data, this.grafika.exts.getCanvasBlob());
            }).then((res) => {
                if (exit) this.exit();
                this.appCommon.toast('Successfully saved!');
            });
		}

        confirmExit() {
            this.appCommon.confirm('Close?').then(() => this.exit());
        }

        exit() {
            this.appCommon.$state.go('my-animations');
        }
    }
}