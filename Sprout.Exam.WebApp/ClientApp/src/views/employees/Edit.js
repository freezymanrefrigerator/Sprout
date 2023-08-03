import React, { Component } from 'react';
import authService from '../../components/api-authorization/AuthorizeService';

export class EmployeeEdit extends Component {
    static displayName = EmployeeEdit.name;

    constructor(props) {
        super(props);
        this.state = { id: 0, fullName: '', birthdate: '', tin: '', employeeTypeId: 1, loading: true, loadingSave: false, originalTin: '' };
    }

    componentDidMount() {
        this.getEmployee(this.props.match.params.id);
    }

    handleChange(event) {
        this.setState({ [event.target.name]: event.target.value });
    }

    async handleSubmit(e) {
        e.preventDefault();

        const currentDate = new Date();
        currentDate.setHours(0, 0, 0, 0);
        const selectedDate = new Date(this.state.birthdate);

        if (!this.state.fullName.trim()) {
            alert("Full name must not be blank");
            return;
        }

        const fullNameRegex = /^[a-zA-Z\s]+$/;
        if (!fullNameRegex.test(this.state.fullName)) {
            alert("Full name must not contain special characters");
            return;
        }

        const tinRegex = /^\d{9}$/; // It should be exactly 9 digits long
        if (!tinRegex.test(this.state.tin)) {
            alert("TIN must be exactly 9 digits and contain only numbers");
            return;
        }

        if (!this.state.tin.trim()) {
            alert("TIN must not be blank");
            return;
        }

        if (!this.state.birthdate) {
            alert("Birthdate must be filled");
            return;
        }

        if (selectedDate > currentDate) {
            alert("Birthdate must not be a future date");
            return;
        }

        if (this.state.tin !== this.state.originalTin && await this.checkTinExists(this.state.tin)) {
            alert("The updated TIN already exists for another employee.");
            return;
        }

        if (window.confirm("Are you sure you want to save?")) {
            this.saveEmployee();
        }
    }


    async checkTinExists(tin) {
        const token = await authService.getAccessToken();
        const response = await fetch(`api/employees/CheckTin/${tin}`, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });

        if (response.ok) {
            const tinExists = await response.json();
            return tinExists;
        } else {
            const errorData = await response.json();
            throw new Error(`Server responded with status: ${response.status} ${errorData}`);
        }
    }


    render() {

        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : <div>
                <form>
                    <div className='form-row'>
                        <div className='form-group col-md-6'>
                            <label htmlFor='inputFullName4'>Full Name: *</label>
                            <input type='text' className='form-control' id='inputFullName4' onChange={this.handleChange.bind(this)} name="fullName" value={this.state.fullName} placeholder='Full Name' />
                        </div>
                        <div className='form-group col-md-6'>
                            <label htmlFor='inputBirthdate4'>Birthdate: *</label>
                            <input type='date' className='form-control' id='inputBirthdate4' onChange={this.handleChange.bind(this)} name="birthdate" value={this.state.birthdate} placeholder='Birthdate' />
                        </div>
                    </div>
                    <div className="form-row">
                        <div className='form-group col-md-6'>
                            <label htmlFor='inputTin4'>TIN: *</label>
                            <input type='number'  className='form-control' id='inputTin4' onChange={this.handleChange.bind(this)} value={this.state.tin} name="tin" placeholder='TIN' />
                        </div>
                        <div className='form-group col-md-6'>
                            <label htmlFor='inputEmployeeType4'>Employee Type: *</label>
                            <select id='inputEmployeeType4' onChange={this.handleChange.bind(this)} value={this.state.employeeTypeId} name="employeeTypeId" className='form-control'>
                                <option value='1'>Regular</option>
                                <option value='2'>Contractual</option>
                            </select>
                        </div>
                    </div>
                    <button type="submit" onClick={this.handleSubmit.bind(this)} disabled={this.state.loadingSave} className="btn btn-primary mr-2">{this.state.loadingSave ? "Loading..." : "Save"}</button>
                    <button type="button" onClick={() => this.props.history.push("/employees/index")} className="btn btn-primary">Back</button>
                </form>
            </div>;

        return (
            <div>
                <h1 id="tabelLabel" >Employee Edit</h1>
                <p>All fields are required</p>
                {contents}
            </div>
        );
    }

    async saveEmployee() {
        this.setState({ loadingSave: true });
        const token = await authService.getAccessToken();

        const employee = {
            id: this.state.id,
            fullName: this.state.fullName,
            birthdate: this.state.birthdate,
            tin: this.state.tin,
            EmployeeTypeId: parseInt(this.state.employeeTypeId)
        };

        const requestOptions = {
            method: 'PUT',
            headers: !token ? {} : { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' },
            body: JSON.stringify(employee)
        };
        const response = await fetch('api/employees/' + this.state.id, requestOptions);

        if (response.status === 200) {
            this.setState({ loadingSave: false });
            alert("Employee successfully saved");
            this.props.history.push("/employees/index");
        }
        else {
            alert("There was an error occurred please try again later");
        }
    }


    async getEmployee(id) {
        this.setState({ loading: true, loadingSave: false });
        const token = await authService.getAccessToken();
        const response = await fetch('api/employees/' + id, {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        const data = await response.json();

        // Get the birthdate as a UTC date object
        const birthdateUTC = new Date(data.birthdate);

        // Get the time zone offset in minutes
        const timezoneOffset = birthdateUTC.getTimezoneOffset();

        // Adjust the date by adding the time zone offset in minutes
        const birthdateLocal = new Date(birthdateUTC.getTime() - timezoneOffset * 60000);

        // Format the local date as a string (e.g., "YYYY-MM-DD")
        const formattedBirthdate = birthdateLocal.toISOString().slice(0, 10);

        this.setState({
            id: data.id,
            fullName: data.fullName,
            birthdate: formattedBirthdate,
            tin: data.tin,
            employeeTypeId: data.employeeTypeId,
            originalTin: data.tin,
            loading: false,
            loadingSave: false
        });
    }

}
