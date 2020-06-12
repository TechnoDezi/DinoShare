(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory() :
        typeof define === 'function' && define.amd ? define(factory) :
            (global.VueSummernote = factory());
}(this, (function () {
    var index = {
        name: 'summernote',
        props: {
            model: {
                required: true
            },
            name: {
                type: String,
                required: true
            },
            config: {
                type: Object,
                default: function () {
                    return {
                        height: 50
                    };
                }
            }
        },
        mounted() {
            let vm = this;
            let config = this.config;

            config.callbacks = {

                onInit: function () {
                    $(vm.$el).summernote("code", vm.model);
                },

                onChange: function () {
                    vm.$emit('change', $(vm.$el).summernote('code'));
                },

                onBlur: function () {
                    vm.$emit('change', $(vm.$el).summernote('code'));
                }
            };

            $(this.$el).summernote(config);

        },
        methods: {
            
        },
        template: '<textarea :name="name"></textarea>'
    };
    return index;
})));