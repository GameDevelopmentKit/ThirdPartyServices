#import <UIKit/UIKit.h>

extern "C" {
    float _GetScreenScale() {
        return [[UIScreen mainScreen] scale];
    }
}