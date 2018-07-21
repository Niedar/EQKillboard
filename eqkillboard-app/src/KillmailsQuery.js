import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"

export const KillmailDataFragment = gql`
fragment KillmailData on Killmail {
  nodeId
  id,
  characterByVictimId {
    id,
    name
  },
  guildByVictimGuildId {
    id, 
    name
  },
  victimLevel,
  characterByAttackerId {
    id,
    name
  },
  guildByAttackerGuildId {
    id,
    name
  },
  attackerLevel,
  zoneByZoneId {
    id,
    name
  },
  killedAt  
}
`;

const GET_ALLKILLMAILS = gql`
  {
    allKillmails(orderBy: KILLED_AT_DESC, first: 50) {
      nodes {
        ...KillmailData
      }    
    }
  }
  ${KillmailDataFragment}
`;

export default class KillmailsQuery extends Component {
  render() {
    const { character_id, guild_id, zone_id, children } = this.props
    var query;
    var id;

    if (character_id) {

    } else if (guild_id) {

    } else if (zone_id) {

    } else {
      query = GET_ALLKILLMAILS;
    }

    return (
      <Query query={query} variables={{ id }}>
        {result => children(result)}
      </Query>
    )
  }
}

const query = gql`
  query User($id: String!) {
    user(id: $id) {
      id
      name
    }
  }
`