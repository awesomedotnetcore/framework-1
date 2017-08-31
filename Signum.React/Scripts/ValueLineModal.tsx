﻿import * as React from 'react'
import { Modal, ModalProps, ModalClass, ButtonToolbar } from 'react-bootstrap'
import { openModal, IModalProps } from './Modals';
import { Dic } from './Globals';
import { SelectorMessage, JavascriptMessage } from './Signum.Entities'
import { TypeInfo, TypeReference, Binding } from './Reflection'
import { FormGroupStyle, TypeContext } from './TypeContext'
import { ValueLineType, ValueLine } from './Lines/ValueLine'
import { ValueLineProps } from "./Lines";
import { ValidationMessage } from "./Signum.Entities";


interface ValueLineModalProps extends React.Props<ValueLineModal>, IModalProps {

    options: ValueLineModalOptions;
}

export default class ValueLineModal extends React.Component<ValueLineModalProps, { show: boolean; value?: any }>  {

    constructor(props: ValueLineModalProps) {
        super(props);

        this.state = {
            show: true,
            value: this.props.options.initialValue
        };
    }

    selectedValue: any = undefined;
    handleOkClick = () => {
        this.selectedValue = this.state.value;
        this.setState({ show: false });
    }

    handleCancelClicked = () => {
        this.selectedValue = undefined;
        this.setState({ show: false });   
    }

    handleOnExited = () => {
        this.props.onExited!(this.selectedValue);
    }

    render() {
    
        const ctx = new TypeContext(undefined, undefined, undefined as any, Binding.create(this.state, s => s.value));

        const { title, message, initialValue, ...valueLineProps } = this.props.options;
        var vlp: ValueLineProps = {
            ctx: ctx,
            formatText: valueLineProps.formatText,
            unitText: valueLineProps.unitText,
            labelText: valueLineProps.labelText,
            type: valueLineProps.type,
            valueLineType: valueLineProps.valueLineType,
        };

        const disabled = this.props.options.allowEmptyValue == false ? (ctx.value as string).trim() ? false : true : undefined;
        const valueOnChanged = this.props.options.allowEmptyValue == false ? () => this.forceUpdate() : undefined;

        return <Modal bsSize="lg" onHide={this.handleCancelClicked} show={this.state.show} onExited={this.handleOnExited}>

            <Modal.Header closeButton={true}>
                <h4 className="modal-title">
                    {title === undefined ? SelectorMessage.ChooseAValue.niceToString() : title}
                </h4>
            </Modal.Header>

            <Modal.Body>
                <p>
                    {message === undefined ? SelectorMessage.PleaseChooseAValueToContinue.niceToString() : message}
                </p>
                <ValueLine ctx={ctx}
                    formGroupStyle={valueLineProps.labelText ? "Basic" : "SrOnly"} {...vlp} onChange={valueOnChanged} />
            </Modal.Body>
            <Modal.Footer>
                <button disabled={disabled} className="btn btn-primary sf-entity-button sf-ok-button" onClick={this.handleOkClick}>
                    {JavascriptMessage.ok.niceToString()}
                </button>
                <button className="btn btn-default sf-entity-button sf-close-button" onClick={this.handleCancelClicked}>
                    {JavascriptMessage.cancel.niceToString()}
                </button>
            </Modal.Footer>
        </Modal>;
    }

    static show(options: ValueLineModalOptions): Promise<any> {
        return openModal<any>(<ValueLineModal options={options}/>);
    }
}

export interface ValueLineModalOptions {
    type: TypeReference;
    valueLineType?: ValueLineType;
    initialValue?: any;
    title?: React.ReactChild;
    message?: React.ReactChild;
    labelText?: React.ReactChild;
    formatText?: string;
    unitText?: string;
    initiallyFocused?: boolean;
    valueHtmlAttributes?: React.HTMLAttributes<any>;
    allowEmptyValue?: boolean;
}


