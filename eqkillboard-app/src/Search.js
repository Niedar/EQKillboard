import React, { Component } from 'react';
import { Icon, Input, AutoComplete } from 'antd';
import { withApollo } from 'react-apollo';
import { withRouter } from 'react-router-dom'
import gql from "graphql-tag"

const Option = AutoComplete.Option;
const OptGroup = AutoComplete.OptGroup;

function renderTitle(title) {
  return (
    <span>
      {title}
    </span>
  );
}

const createOptions = dataSource =>  {
    return dataSource.map(group => (
    <OptGroup
        key={group.title}
        label={renderTitle(group.title)}
    >
        {group.children.map(opt => (
        <Option key={opt.title} value={opt.title} character_id={opt.character_id} guild_id={opt.guild_id}>
            {opt.title}
        </Option>
        ))}
    </OptGroup>
    ))
};

class Search extends React.Component {
    constructor() {
        super();

        this.state= {
            value: '',
            suggestions: []
        }
    }
    onSearch = value => {
        this.props.client.query({
            query: GET_SEARCHALL,
            variables: { searchText: value}
        }).then(result => {
            var self = this;
            let suggestions = [];
            if (result.data) {
                var characterGroup = {
                    title: "Characters",
                    children: result.data.searchCharacters.nodes.map(node => {
                        return {
                            character_id: node.id,
                            title: node.name,
                            count: 1000
                        }
                    })
                }
                var guildGroup =  {
                    title: "Guilds",
                    children: result.data.searchGuilds.nodes.map(node => {
                        return {
                            guild_id: node.id,
                            title: node.name,
                            count: 1000
                        }
                    })
                }
                suggestions = [characterGroup, guildGroup]
            }
            self.setState({
                suggestions
            });
        })
        this.setState({
          value
        })
    }
    onSelect = (value, option) => {
      console.log(value);
      console.log(option);

      if (option.props.character_id) {
        this.props.history.push(`/character/${option.props.character_id}`);
      } else if (option.props.guild_id) {
        this.props.history.push(`/guild/${option.props.guild_id}`);
      }
      this.setState({
        value: ''
      })
    }
    render() {
        return (
            <div className="certain-category-search-wrapper" style={{ width: 300, float: "right" }}>
              <AutoComplete
                className="certain-category-search"
                dropdownClassName="certain-category-search-dropdown"
                dropdownMatchSelectWidth={false}
                dropdownStyle={{ width: 300 }}
                size="large"
                style={{ width: '100%' }}
                dataSource={createOptions(this.state.suggestions)}
                placeholder="Search..."
                optionLabelProp="value"
                value={this.state.value}
                onSearch={this.onSearch}
                onSelect={this.onSelect}
              >
                <Input suffix={<Icon type="search" className="certain-category-icon" />} />
              </AutoComplete>
            </div>
          );
    }
}

const GET_SEARCHALL = gql`
query searchAll($searchText: String) {
    searchCharacters(search: $searchText) {
      nodes {
        id,
        name
      }
    },
    searchGuilds(search: $searchText) {
      nodes {
        id,
        name
      }
    },
  }
`

export default withRouter(withApollo(Search));